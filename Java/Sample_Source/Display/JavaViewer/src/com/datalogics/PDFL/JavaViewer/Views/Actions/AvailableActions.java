/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

/*
 * AvailableActions - it is an enumeration which contains most of the used
 * actions with their icons and key accelerators.
 */

import java.awt.event.InputEvent;
import java.awt.event.KeyEvent;

import javax.swing.ImageIcon;
import javax.swing.KeyStroke;

public enum AvailableActions {
    ACTION_OPEN("Open", "FileOpenMenu.gif", KeyStroke.getKeyStroke(KeyEvent.VK_O, InputEvent.CTRL_MASK)), ACTION_SAVE("Save", "FileSaveMenu.gif", KeyStroke.getKeyStroke(KeyEvent.VK_S, InputEvent.CTRL_MASK)), ACTION_PRINT("Print", "PrintMenu.gif", KeyStroke.getKeyStroke(KeyEvent.VK_P, InputEvent.CTRL_MASK)), ACTION_SAVE_AS("Save as...", "FileSaveMenu.gif"), ACTION_APPEND("Append"), ACTION_UNLOCK("Unlock"), ACTION_CLOSE("Close", KeyStroke.getKeyStroke(KeyEvent.VK_C, InputEvent.CTRL_MASK)),
    ACTION_ZOOM_INC("Increase zoom", "ZoomIncreaseMenu.gif"), ACTION_ZOOM_DEC("Decrease zoom", "ZoomDecreaseMenu.gif"),
    ACTION_ZOOM_FIT_PAGE("Fit to page"), ACTION_ZOOM_FIT_WIDTH("Fit to width"), ACTION_ZOOM_FIT_HEIGHT("Fit to height"),
    ACTION_FIRST_PAGE("go to first page", "FirstPageMenu.gif"), ACTION_PREV_PAGE("Go to previous page", "PreviousPageMenu.gif"), ACTION_NEXT_PAGE("Go to next page", "NextPageMenu.gif"), ACTION_LAST_PAGE("go to last page", "LastPageMenu.gif"),
    ACTION_SINGLE_PAGE_MODE("Single page mode", KeyStroke.getKeyStroke(KeyEvent.VK_S, InputEvent.CTRL_MASK | InputEvent.ALT_MASK)), ACTION_CONTINUOUS_PAGE_MODE("Continuous mode", KeyStroke.getKeyStroke(KeyEvent.VK_C, InputEvent.CTRL_MASK | InputEvent.ALT_MASK)),
    ACTION_DEFAULT_MODE("Edit mode", "SelectModeMenu.gif", "SelectModeSelected.gif", null), ACTION_MARQUEE_ZOOM_MODE("Marquee zoom mode", "MarqueeZoomModeMenu.gif", "MarqueeZoomModeMenuSelected.gif", null), ACTION_ZOOM_MODE("Zoom mode", "ZoomModeMenu.gif", "ZoomModeMenuSelected.gif", null),
    ACTION_TEXTSEARCH("Text Search", "SearchTextMenu.gif"),
    ACTION_REDO("Redo", "ReDo.gif", KeyStroke.getKeyStroke(KeyEvent.VK_Y, InputEvent.CTRL_MASK)), ACTION_UNDO("Undo", "UnDo.gif", KeyStroke.getKeyStroke(KeyEvent.VK_Z, InputEvent.CTRL_MASK)),
    ACTION_CIRCLE_ANNOTATION("Circle", "EllipseMenu.gif", "EllipseMenuSelected.gif", null), ACTION_LINE_ANNOTATION("Line", "LineMenu.gif", "LineMenuSelected.gif", null), ACTION_ARROW_ANNOTATION("Arrow", "ArrowMenu.gif", "ArrowMenuSelected.gif", null), ACTION_SQUARE_ANNOTATION("Square", "RectangleMenu.gif", "RectangleMenuSelected.gif", null), ACTION_POLYLINE_ANNOTATION("Polyline", "PolyLineMenu.gif", "PolyLineMenuSelected.gif", null), ACTION_POLYGON_ANNOTATION("Polygon", "PolygonMenu.gif", "PolygonMenuSelected.gif", null), ACTION_FREETEXT_ANNOTATION("Free text", "FreeTextMenu.gif", "FreeTextMenuSelected.gif", null), ACTION_INK_ANNOTATION("Ink", "Ink.gif", "InkChoice.gif", null), ACTION_LINK_ANNOTATION("Link", "Link.gif", "LinkChoice.gif", null), ACTION_HIGHLIGHT_ANNOTATION("Highlight", "Highlight.gif", "HighlightChoice.gif", null), ACTION_UNDERLINE_ANNOTATION("Underline", "Underline.gif", "UnderlineChoice.gif", null),
    ACTION_FONT_FACE("Family"), ACTION_FONT_SIZE("Size"), ACTION_FONT_JUSTIFICATION("Justification"),
    ACTION_LINE_STYLE_SOLID("Solid", "SolidLineMenu.gif"), ACTION_LINE_STYLE_DASHED1("Dashed 1", "Dashed1LineMenu.gif"), ACTION_LINE_STYLE_DASHED2("Dashed 2", "Dashed2LineMenu.gif"), ACTION_LINE_STYLE_DASHED3("Dashed 3", "Dashed3LineMenu.gif"), ACTION_LINE_STYLE_DASHED4("Dashed 4", "Dashed4LineMenu.gif"),
    ACTION_LINE_WIDTH1("1 pt.", "LineWeight1Menu.gif"), ACTION_LINE_WIDTH2("2 pt.", "LineWeight2Menu.gif"), ACTION_LINE_WIDTH3("3 pt.", "LineWeight3Menu.gif"), ACTION_LINE_WIDTH5("5 pt.", "LineWeight4Menu.gif"), ACTION_LINE_WIDTH_OTHER("Other...", "LineWeight4Menu.gif"),
    ACTION_ANNOTATION_COLOR("Color"), ACTION_MENU_COLOR("Color"),
    ACTION_START_ARROW_NONE("None"), ACTION_START_ARROW_OPEN("Open Arrow"), ACTION_START_ARROW_CLOSED("Close Arrow"), ACTION_START_ARROW_CIRCLE("Circle Arrow"), ACTION_END_ARROW_NONE("None"), ACTION_END_ARROW_OPEN("Open Arrow"), ACTION_END_ARROW_CLOSED("Close Arrow"), ACTION_END_ARROW_CIRCLE("Circle Arrow"),
    ACTION_DELETE_ANNOTATION("Delete"), ACTION_ANNOTATION_PROPERTIES("Properties"), ACTION_AUTOSIZE("Autosize vertically"), ACTION_BORDER("Show border"),
    ACTION_RESOLUTION("Monitor Resolution");

    AvailableActions(String name, String iconName, String selectedIconName, KeyStroke accelerator) {
        final String resourcePath = "/com/datalogics/PDFL/JavaViewer/Resources/";
        this.name = name;
        this.iconResource = new ImageIcon(getClass().getResource(resourcePath + iconName));
        this.selectedIconResource = new ImageIcon(getClass().getResource(resourcePath + selectedIconName));
        this.accelerator = accelerator;
    }

    AvailableActions(String name, String iconName, KeyStroke accelerator) {
        this(name, iconName, "", accelerator);
    }

    AvailableActions(String name, KeyStroke accelerator) {
        this(name, "", "", accelerator);
    }

    AvailableActions(String name, String iconName) {
        this(name, iconName, "", null);
    }

    AvailableActions(String name) {
        this(name, "", "", null);
    }

    public String getName() {
        return name;
    }

    public ImageIcon getIconResource() {
        return iconResource;
    }

    public ImageIcon getSelectedIconResource() {
        return selectedIconResource;
    }

    public KeyStroke getAccelerator() {
        return accelerator;
    }

    private String name;
    private ImageIcon iconResource;
    private ImageIcon selectedIconResource;
    private KeyStroke accelerator;
}
