/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

import java.awt.Point;
import java.util.EnumSet;

import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ApplicationMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.DialogMenuItem;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.MenuType;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.PageViewMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;

public interface Viewer {
    Dialogs getDialogs();

    PDF getPDF();

    Bookmarks getBookmarks();

    Layers getLayers();

    void showDialogMenu(EnumSet<DialogMenuItem> items, final DialogMenuResultNotifier subscriber, Point location);

    void showContextMenu(MenuType menuType, Point location);

    void updateControls();

    void updateZoom(ZoomMode mode, double zoom);

    void updatePage(int index, int count);

    void updateViewMode(PageViewMode mode);

    void updateUnlock(boolean state);

    void updateTitle(String title);

    void updateApplicationMode(ApplicationMode applicationMode);

    void updateUndoRedoButtons(boolean hasUndo, boolean hasRedo);

    void updateLineStyleButton(float[] dashPattern);

    void updateLineWidthButton(float width);

    void updateColorButton(ColorPicker colors);

    void updateArrowMenus(String subType, LineEndingStyle startStyle, LineEndingStyle endStyle);

    void updateAnnotationButton(String subType);

    void updateFontProperty(String fonface, int fontSize, HorizontalAlignment alignment);
}
