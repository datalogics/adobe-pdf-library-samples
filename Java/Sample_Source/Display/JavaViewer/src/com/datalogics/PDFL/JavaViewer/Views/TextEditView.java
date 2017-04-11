/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * TextEditView - is used for creating richTextbox for FreeTextAnnotation
 * annotation. This class sets text and background colors, font size and font
 * face.
 */

import java.awt.Color;
import java.awt.Font;
import java.awt.Point;
import java.awt.Rectangle;

import javax.swing.JComponent;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;

import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.TextEdit;

public class TextEditView extends JTextArea implements TextEdit {
    public TextEditView(Rectangle boundRect, JComponent pdfPanel) {
        // default view parameters
        this.fontFace = "Arial";
        this.fontSize = 10;
        this.bgColor = Color.white;
        this.color = Color.black;

        this.pdfPanel = pdfPanel;
        this.scrollPane = new JScrollPane(this);
        this.pdfPanel.add(scrollPane);
        this.requestFocus();

        this.offset = new Point();
        setViewBounds(boundRect);
    }

    public void processInput(InputData input) {
        input.markProcessed();
    }

    public Rectangle getViewBounds() {
        return new Rectangle(originalBounds);
    }

    public void setViewBounds(Rectangle bounds) {
        setViewBounds(bounds, false);
    }

    public String getText() {
        return super.getText();
    }

    public void removeTextEdit() {
        pdfPanel.remove(scrollPane);
    }

    public void setBgColor(Color bgColor) {
        super.setBackground(bgColor);
    }

    public void setFontColor(Color color) {
        super.setForeground(color);
    }

    public void setFontFace(String fontFace) {
        this.fontFace = fontFace;
        updateFont();
    }

    public void setFontSize(int fontSize) {
        this.fontSize = fontSize;
        updateFont();
    }

    public void setFontAlign(HorizontalAlignment align) {
    }

    public void setText(String text) {
        super.setText(text);
    }

    void setBoundsOffset(Point offset) {
        this.offset = new Point(offset);
        setViewBounds(getViewBounds(), true); // reset bounds
    }

    private void setViewBounds(Rectangle bounds, boolean onlyOffset) {
        this.originalBounds = new Rectangle(bounds); // keep original bounds
        Rectangle corrected = getViewBounds();
        corrected.translate(offset.x, offset.y);

        // the operations needed for no-layout panel (refer to corresponding Java tutorial)
        if (onlyOffset) {
            scrollPane.setLocation(corrected.getLocation());
        } else {
            setBounds(corrected);
            scrollPane.setBounds(corrected);
        }
        scrollPane.repaint();
    }

    private void updateFont() {
        super.setFont(new Font(fontFace, Font.PLAIN, fontSize));
    }

    private String fontFace;
    private int fontSize;
    Color bgColor;
    Color color;

    private final JScrollPane scrollPane;
    private final JComponent pdfPanel;

    private Rectangle originalBounds;
    private Point offset;
}
