/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer;

import java.awt.event.ActionListener;
import java.awt.event.ItemListener;

import javax.swing.ButtonGroup;
import javax.swing.ButtonModel;
import javax.swing.event.ChangeListener;

/**
 * DropDownButtonModel - represents button model which allows acting of the
 * button as menu button (analogous to .Net WinForms ToolStripButton).
 */
public class DropDownButtonModel implements ButtonModel {
    public DropDownButtonModel(ButtonModel mainButton, ButtonModel childButton) {
        this.mainButton = mainButton;
        this.childButton = childButton;
    }

    public void addActionListener(ActionListener listener) {
        mainButton.addActionListener(listener);
        childButton.addActionListener(listener);
    }

    public void addChangeListener(ChangeListener listener) {
        mainButton.addChangeListener(listener);
        childButton.addChangeListener(listener);
    }

    public void addItemListener(ItemListener listener) {
        mainButton.addItemListener(listener);
        childButton.addItemListener(listener);
    }

    public String getActionCommand() {
        return null;// mainButton.getActionCommand();
    }

    public int getMnemonic() {
        return mainButton.getMnemonic();
    }

    public Object[] getSelectedObjects() {
        return mainButton.getSelectedObjects();
    }

    public boolean isArmed() {
        return mainButton.isArmed();
    }

    public boolean isEnabled() {
        return mainButton.isEnabled();
    }

    public boolean isPressed() {
        return mainButton.isPressed();
    }

    public boolean isRollover() {
        return mainButton.isRollover();
    }

    public boolean isSelected() {
        return mainButton.isSelected();
    }

    public void removeActionListener(ActionListener listener) {
        mainButton.removeActionListener(listener);
        childButton.removeActionListener(listener);
    }

    public void removeChangeListener(ChangeListener listener) {
        mainButton.removeChangeListener(listener);
        childButton.removeChangeListener(listener);
    }

    public void removeItemListener(ItemListener listener) {
        mainButton.removeItemListener(listener);
        childButton.removeItemListener(listener);
    }

    public void setActionCommand(String actionCommand) {
        this.actionCommand = actionCommand;
    }

    public void setArmed(boolean armed) {
        mainButton.setArmed(armed);
        childButton.setArmed(armed);
    }

    public void setEnabled(boolean enable) {
        mainButton.setEnabled(enable);
        childButton.setEnabled(enable);
    }

    public void setGroup(ButtonGroup group) {
        mainButton.setGroup(group);
        childButton.setGroup(group);
    }

    public void setMnemonic(int key) {
        mainButton.setMnemonic(key);
        childButton.setMnemonic(key);
    }

    public void setPressed(boolean pressed) {
        mainButton.setPressed(pressed);
        childButton.setPressed(pressed);
    }

    public void setRollover(boolean rollover) {
        mainButton.setRollover(rollover);
        childButton.setRollover(rollover);
    }

    public void setSelected(boolean select) {
        mainButton.setSelected(select);
        childButton.setSelected(select);
    }

    private ButtonModel mainButton;
    private ButtonModel childButton;
    private String actionCommand;
}
