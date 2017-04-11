/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation;

/*
 * KeyNavigationPresenter is responsible for the navigation in the document. It
 * uses scroll from the Scroll interface to set current page and scroll position
 * on the page, also it uses NavigateCommand for Page Up, Page Down and Home,
 * End navigation.
 */

import java.awt.event.KeyEvent;

import com.datalogics.PDFL.JavaViewer.Document.Command.NavigateCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.NavigateMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.BaseInteractive;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData.ControlKey;

/**
 * KeyNavigationPresenter - responsible for the navigation from the keyboard in
 * the document.
 * 
 * It uses scroll from the Scroll interface to set current page and scroll
 * position on the page, it also uses NavigateCommand for Page Up, Page Down and
 * Home, End navigation.
 */
public class KeyNavigationPresenter extends BaseInteractive {
    @Override
    public void keyPressed(InputData input) {
        boolean vertHorz = true;
        int scrollPosition = 0;
        boolean needNavigate = true;
        NavigateCommand command = null;

        if (input.getControlKey(ControlKey.UP)) {
            scrollPosition -= navigateStep;
        } else if (input.getControlKey(ControlKey.DOWN)) {
            scrollPosition += navigateStep;
        } else if (input.getControlKey(ControlKey.LEFT)) {
            vertHorz = false;
            scrollPosition -= navigateStep;
        } else if (input.getControlKey(ControlKey.RIGHT)) {
            vertHorz = false;
            scrollPosition += navigateStep;
        } else if (input.getControlKey(ControlKey.PAGEUP)) {
            command = new NavigateCommand(NavigateMode.NAVIGATE_PREV, getActivePage());
        } else if (input.getControlKey(ControlKey.PAGEDOWN)) {
            command = new NavigateCommand(NavigateMode.NAVIGATE_NEXT, getActivePage());
        } else if (input.getControlKey(ControlKey.HOME)) {
            command = new NavigateCommand(NavigateMode.NAVIGATE_FIRST, getActivePage());
        } else if (input.getControlKey(ControlKey.END)) {
            command = new NavigateCommand(NavigateMode.NAVIGATE_LAST, getActivePage());
        } else {
            needNavigate = false;
        }

        getApplication().executeCommand(command);
        if (needNavigate && command == null) {
            scrollPosition += getPdfView().getScroll(vertHorz).getCurrent();
            getPdfView().getScroll(vertHorz).setCurrent(scrollPosition);
        }
    }

    public void setBlockIncrement(int navigateStep) {
        this.navigateStep = navigateStep;
    }

    private int navigateStep;
}
